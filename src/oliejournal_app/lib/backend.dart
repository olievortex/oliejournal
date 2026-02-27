import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:oliejournal_app/models/forecast_model.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';

class AudioUploadRetryInfo {
  const AudioUploadRetryInfo({
    required this.failedAttempt,
    required this.maxAttempts,
    required this.retryDelay,
    required this.error,
  });

  final int failedAttempt;
  final int maxAttempts;
  final Duration retryDelay;
  final Object error;

  int get nextAttempt => failedAttempt + 1;
}

class _UploadHttpException implements Exception {
  const _UploadHttpException(this.statusCode);

  final int statusCode;

  @override
  String toString() => 'Status $statusCode when uploading audio';
}

class Backend {
  static Future<ForecastModel> fetchForecast(String? token) async {
    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/secure/weatherforecast',
    );
    final response = await http.get(
      uri,
      headers: {HttpHeaders.authorizationHeader: 'Bearer $token'},
    );

    if (response.statusCode != 200) {
      throw Exception('Status ${response.statusCode} when getting forecast');
    }

    final json = (jsonDecode(response.body) as List)
        .map((i) => ForecastModel.fromJson(i))
        .toList();
    if (json.isEmpty) {
      throw Exception('Null result when getting forecast');
    }

    final result = json[0];

    return result;
  }

  static Future<List<JournalEntryModel>> fetchJournalEntries(
    String? token,
  ) async {
    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/journal/entries',
    );
    final response = await http.get(
      uri,
      headers: {HttpHeaders.authorizationHeader: 'Bearer $token'},
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Status ${response.statusCode} when getting journal entries',
      );
    }

    final result = (jsonDecode(response.body) as List)
        .map((i) => JournalEntryModel.fromJson(i))
        .toList();
    if (result.isEmpty) {
      throw Exception('Null result when getting journal entries');
    }

    return result;
  }

  static Future<JournalEntryModel> fetchJournalEntry(
    int id,
    String? token,
  ) async {
    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/journal/entries/$id',
    );
    final response = await http.get(
      uri,
      headers: {HttpHeaders.authorizationHeader: 'Bearer $token'},
    );

    if (response.statusCode != 200) {
      throw Exception('Status ${response.statusCode} when getting entry $id');
    }

    final result = JournalEntryModel.fromJson(jsonDecode(response.body));
    return result;
  }

  static Future<void> deleteJournalEntry(
    int id,
    String? token,
  ) async {
    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/journal/entries/$id',
    );
    final response = await http.delete(
      uri,
      headers: {HttpHeaders.authorizationHeader: 'Bearer $token'},
    );

    if (response.statusCode != 204) {
      throw Exception('Status ${response.statusCode} when deleting entry $id');
    }
  }
  
  /// Upload an audio recording file to the server.
  ///
  /// The API expects a multipart POST to `/api/journal/audioEntry` with
  /// a single file field named `file`. A bearer token must be supplied
  /// for authorization. Throws on non-success codes.
  static Future<void> uploadAudioEntry(
    String? token,
    File file, {
    double? latitude,
    double? longitude,
    void Function(AudioUploadRetryInfo retryInfo)? onRetryScheduled,
    int maxAttempts = 4,
  }) async {
    if (token == null) {
      throw Exception('No authentication token available');
    }

    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/journal/audioEntry',
    );

    for (var attempt = 1; attempt <= maxAttempts; attempt++) {
      try {
        await _sendAudioUploadRequest(
          uri: uri,
          token: token,
          file: file,
          latitude: latitude,
          longitude: longitude,
        );
        return;
      } catch (error) {
        final shouldRetry = _isRetriableUploadError(error);
        final hasRetryRemaining = attempt < maxAttempts;
        if (!shouldRetry || !hasRetryRemaining) {
          rethrow;
        }

        final retryDelay = _retryDelayForAttempt(attempt);
        onRetryScheduled?.call(
          AudioUploadRetryInfo(
            failedAttempt: attempt,
            maxAttempts: maxAttempts,
            retryDelay: retryDelay,
            error: error,
          ),
        );
        await Future.delayed(retryDelay);
      }
    }
  }

  static Future<void> _sendAudioUploadRequest({
    required Uri uri,
    required String token,
    required File file,
    double? latitude,
    double? longitude,
  }) async {
    final request = http.MultipartRequest('POST', uri);
    request.headers[HttpHeaders.authorizationHeader] = 'Bearer $token';
    request.files.add(await http.MultipartFile.fromPath('file', file.path));

    if (latitude != null) {
      request.fields['latitude'] = latitude.toString();
    }
    if (longitude != null) {
      request.fields['longitude'] = longitude.toString();
    }

    final streamedResponse = await request.send().timeout(
      const Duration(seconds: 30),
    );
    if (streamedResponse.statusCode < 200 || streamedResponse.statusCode >= 300) {
      throw _UploadHttpException(streamedResponse.statusCode);
    }
  }

  static bool _isRetriableUploadError(Object error) {
    if (error is SocketException ||
        error is TimeoutException ||
        error is HttpException ||
        error is http.ClientException) {
      return true;
    }

    if (error is _UploadHttpException) {
      return error.statusCode == 408 ||
          error.statusCode == 429 ||
          error.statusCode >= 500;
    }

    return false;
  }

  static Duration _retryDelayForAttempt(int attempt) {
    final seconds = attempt == 1
        ? 2
        : attempt == 2
        ? 5
        : attempt == 3
        ? 10
        : 20;
    return Duration(seconds: seconds);
  }
}

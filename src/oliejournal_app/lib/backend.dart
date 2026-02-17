import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:oliejournal_app/models/forecast_model.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';

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
      'https://oliejournal.olievortex.com/api/journal/entry/$id',
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
  }) async {
    if (token == null) {
      throw Exception('No authentication token available');
    }

    final uri = Uri.parse(
      'https://oliejournal.olievortex.com/api/journal/audioEntry',
    );

    final request = http.MultipartRequest('POST', uri);
    request.headers[HttpHeaders.authorizationHeader] = 'Bearer $token';
    request.files.add(await http.MultipartFile.fromPath('file', file.path));

    if (latitude != null) {
      request.fields['latitude'] = latitude.toString();
    }
    if (longitude != null) {
      request.fields['longitude'] = longitude.toString();
    }

    final streamedResponse = await request.send();
    if (streamedResponse.statusCode < 200 || streamedResponse.statusCode >= 300) {
      throw Exception('Status ${streamedResponse.statusCode} when uploading audio');
    }
  }
}

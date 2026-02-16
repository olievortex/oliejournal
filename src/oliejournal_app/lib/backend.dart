import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:oliejournal_app/models/forecast_model.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';

class Backend {
  static const String _forecastUrl =
      'https://oliejournal.olievortex.com/api/secure/weatherforecast';
  static const String _entriesListUrl = 
      'https://oliejournal.olievortex.com/api/journal/entries';

  static Future<ForecastModel> fetchForecast(String? token) async {
    final uri = Uri.parse(_forecastUrl);
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

    static Future<List<JournalEntryModel>> fetchJournalEntries(String? token) async {
    final uri = Uri.parse(_entriesListUrl);
    final response = await http.get(
      uri,
      headers: {HttpHeaders.authorizationHeader: 'Bearer $token'},
    );

    if (response.statusCode != 200) {
      throw Exception('Status ${response.statusCode} when getting journal entries');
    }

    final result = (jsonDecode(response.body) as List)
        .map((i) => JournalEntryModel.fromJson(i))
        .toList();
    if (result.isEmpty) {
      throw Exception('Null result when getting journal entries');
    }

    return result;
  }

  // fetch a single journal entry by id; used by UI timers to avoid
  // refreshing the entire list when only one entry changed.
  static Future<JournalEntryModel> fetchJournalEntry(int id, String? token) async {
    final uri = Uri.parse('https://oliejournal.olievortex.com/api/journal/entryStatus/$id');
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
}

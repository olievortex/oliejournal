import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:oliejournal_app/models/forecast_model.dart';

class Backend {
  static const String _forecastUrl =
      'https://oliejournal.olievortex.com/api/secure/weatherforecast';

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
}

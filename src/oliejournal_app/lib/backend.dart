import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:oliejournal_app/models/forecast_model.dart';

class Backend {
  static const String _forecastUrl = 'https://oliejournal.olievortex.com/api/weatherforecast';

  static Future<ForecastModel> fetchForecast() async {
    final uri = Uri.parse(_forecastUrl);
    final response = await http.get(uri);

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

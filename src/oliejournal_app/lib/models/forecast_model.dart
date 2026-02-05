import 'package:intl/intl.dart';

class ForecastModel {
  final DateTime effectiveDate;
  final int temperatureF;
  final String summary;

  const ForecastModel({
    required this.effectiveDate,
    required this.temperatureF,
    required this.summary,
  });

  factory ForecastModel.fromJson(Map<String, dynamic> json) {
    final format = DateFormat("yyyy-MM-dd");
    
    return ForecastModel(
      effectiveDate: format.parse(json['date']),
      summary: json['summary'] as String,
      temperatureF: json['temperatureF'] as int
    );
  }
}

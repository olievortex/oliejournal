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
    return switch (json) {
      {
        'date': DateTime effectiveDate,
        'temperatureF': int temperatureF,
        'summary': String summary,
      } =>
        ForecastModel(
          effectiveDate: effectiveDate,
          temperatureF: temperatureF,
          summary: summary,
        ),
      _ => throw const FormatException('Failed to load forecast.'),
    };
  }
}

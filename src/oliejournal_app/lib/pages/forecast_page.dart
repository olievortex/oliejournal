import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:provider/provider.dart';

class ForecastPage extends StatelessWidget {
  const ForecastPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const HomeHeader(), elevation: 0),
      body: Padding(
        padding: EdgeInsets.only(
          left: 16,
          right: 16,
          bottom: MediaQuery.viewPaddingOf(context).bottom,
        ),
        child: Column(
          children: [
            const Text("Today's Forecast"),
            verticalSpaceMedium,
            _forecastBody(),
            const Spacer(),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }

  Widget _forecastBody() {
    return Consumer<OlieModel>(
      builder: (context, olieModel, child) {
        return olieModel.isLoading
            ? const CircularProgressIndicator.adaptive()
            : olieModel.forecast == null
            ? _missingForecast(olieModel)
            : _forecast(olieModel);
      },
    );
  }

  Widget _forecast(OlieModel olieModel) {
    final formatter = DateFormat('yyyy-MM-dd');
    final String effectiveDate = formatter.format(
      olieModel.forecast?.effectiveDate ?? DateTime.now(),
    );

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: EdgeInsets.all(32),
          decoration: roundedBoxRegular,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              verticalSpaceMedium,
              Text(
                "Woohoo!",
                textAlign: TextAlign.center,
                style: kTitleText.copyWith(color: Colors.white),
              ),
              verticalSpaceMedium,
              Text(
                olieModel.forecast?.summary ?? "Fair",
                textAlign: TextAlign.center,
                style: kRobotoText.copyWith(
                  fontWeight: kFwBlack,
                  color: Colors.white,
                  fontSize: kTitleLarge,
                ),
              ),
            ],
          ),
        ),
        verticalSpaceRegular,
        Center(
          child: Column(
            children: [
              Text(
                "High Temperature: ${olieModel.forecast?.temperatureF}F",
                style: kTitleText,
              ),
              Text(
                "Date: $effectiveDate",
                style: kTitleText.copyWith(fontSize: kBodySmall),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _missingForecast(OlieModel olieModel) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(32),
      decoration: roundedBoxRegular,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          verticalSpaceMedium,
          Text(
            "We couldn't\nfetch the latest\nforecast",
            textAlign: TextAlign.center,
            style: kRobotoText.copyWith(
              fontWeight: kFwBlack,
              color: Colors.white,
              fontSize: kTitleLarge,
            ),
          ),
          verticalSpaceMedium,
          Text(
            olieModel.errorMessage ?? "There was an unexpected error",
            textAlign: TextAlign.center,
            style: kTitleText.copyWith(
              fontWeight: kFwBlack,
              color: Colors.white,
            ),
          ),
          verticalSpaceMedium,
          MaterialButton(
            elevation: 0,
            color: Colors.white,
            onPressed: () {
              olieModel.fetchForecast();
            },
            child: Text(
              'Try again',
              textAlign: TextAlign.center,
              style: kRobotoText.copyWith(
                fontWeight: kFwBlack,
                color: Colors.black,
                fontSize: kHeadingTwo,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

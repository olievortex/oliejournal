import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/forecast_page.dart';
import 'package:oliejournal_app/pages/voice_recording_page.dart';
import 'package:provider/provider.dart';
import 'package:oliejournal_app/pages/journal_entries_page.dart';

class HomeBody extends StatelessWidget {
  const HomeBody({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<OlieModel>(
      builder: (context, olieModel, child) {
        return olieModel.isLoggedIn
            ? _loggedInContent(context, olieModel)
            : _initialContent();
      },
    );
  }

  Widget _initialContent() {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(24),
      decoration: roundedBoxRegular,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Icon(
            Icons.storm,
            size: 56,
            color: Colors.white,
          ),
          const SizedBox(height: 20),
          Text(
            "Storm chase journaling made simple",
            textAlign: TextAlign.center,
            style: kRobotoText.copyWith(
              fontWeight: kFwBlack,
              color: Colors.white,
              fontSize: kTitleLarge,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            "Record voice notes, generate transcripts, and save location context for every chase.",
            textAlign: TextAlign.center,
            style: kRobotoText.copyWith(
              color: Colors.white,
              fontSize: kHeadingTwo,
            ),
          ),
          const SizedBox(height: 20),
          Text(
            "Create your free account today — no credit card required.",
            textAlign: TextAlign.center,
            style: kTitleText.copyWith(
              fontWeight: kFwBlack,
              color: Colors.white,
            ),
          ),
        ],
      ),
    );
  }

  Widget _loggedInContent(BuildContext context, OlieModel olieModel) {
    // modern storm‑chaser theme: dark gradient header, icon buttons and
    // language that reflects chasing storms in the field
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: EdgeInsets.all(32),
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [Colors.grey.shade900, Colors.black],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              verticalSpaceMedium,
              Text(
                "Welcome back, ${olieModel.nameFirst}",
                textAlign: TextAlign.center,
                style: kTitleText.copyWith(color: Colors.white),
              ),
              verticalSpaceMedium,
              Text(
                "You're logged in and ready to\ndocument your\nstorm chase.",
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
        // only show forecast button in debug builds
        if (kDebugMode) ...[
          ElevatedButton.icon(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.black,
              foregroundColor: Colors.white,
              elevation: 0,
              padding: EdgeInsets.symmetric(vertical: 16, horizontal: 24),
            ),
            icon: Icon(Icons.cloud, size: 24),
            label: Text("Load forecast"),
            onPressed: () {
              olieModel.fetchForecast();
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const ForecastPage()),
              );
            },
          ),
          verticalSpaceRegular,
        ],
        ElevatedButton.icon(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.black,
            foregroundColor: Colors.white,
            elevation: 0,
            padding: EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          ),
          icon: Icon(Icons.create, size: 24),
          label: Text("Create a field report"),
          onPressed: () {
            Navigator.push(
              context,
              MaterialPageRoute(builder: (context) => const VoiceRecordingPage()),
            );
          },
        ),
        verticalSpaceRegular,
        ElevatedButton.icon(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.black,
            foregroundColor: Colors.white,
            elevation: 0,
            padding: EdgeInsets.symmetric(vertical: 16, horizontal: 24),
          ),
          icon: Icon(Icons.list, size: 24),
          label: Text("View entries"),
          onPressed: () {
            olieModel.fetchEntries();
            Navigator.push(
              context,
              MaterialPageRoute(builder: (context) => const JournalEntriesPage()),
            );
          },
        ),
      ],
    );
  }
}

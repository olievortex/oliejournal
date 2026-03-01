import 'package:flutter/material.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_body.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:oliejournal_app/pages/tutorial/tutorial_page.dart';
import 'package:provider/provider.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  Future<void> _openTutorial(BuildContext context) async {
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => TutorialPage(
          onFinished: () async {
            Navigator.of(context).pop();
          },
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: LayoutBuilder(
        builder: (context, constraints) {
          return SingleChildScrollView(
            child: ConstrainedBox(
              constraints: BoxConstraints(minHeight: constraints.maxHeight),
              child: IntrinsicHeight(
                child: Padding(
                  padding: EdgeInsets.only(
                    top: MediaQuery.viewPaddingOf(context).top,
                    left: 16,
                    right: 16,
                    bottom: MediaQuery.viewPaddingOf(context).bottom,
                  ),
                  child: Column(
                    children: [
                      const HomeHeader(),
                      Consumer<OlieModel>(
                        builder: (context, olieModel, child) {
                          if (olieModel.isLoggedIn) {
                            return const SizedBox.shrink();
                          }

                          return Align(
                            alignment: Alignment.centerRight,
                            child: TextButton(
                              onPressed: () => _openTutorial(context),
                              child: const Text('Tutorial'),
                            ),
                          );
                        },
                      ),
                      verticalSpaceMedium,
                      const HomeBody(),
                      const Spacer(),
                      const HomeFooter(),
                    ],
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
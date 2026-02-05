import 'package:flutter/material.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/pages/home/components/home_body.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: EdgeInsets.only(
          top: MediaQuery.viewPaddingOf(context).top,
          left: 16,
          right: 16,
          bottom: MediaQuery.viewPaddingOf(context).bottom
        ),
        child: Column(
          children: [
            const HomeHeader(),
            verticalSpaceMedium,
            const HomeBody(),
            const Spacer(),
            const HomeFooter(),
          ],
        ),
      )
    );
  }
}
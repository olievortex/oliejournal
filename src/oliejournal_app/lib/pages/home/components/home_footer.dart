import 'package:flutter/material.dart';
import 'package:oliejournal_app/constants.dart';

class HomeFooter extends StatelessWidget {
  const HomeFooter({super.key});

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      child: Column(
        children: [
          Text(
            '© 2026 AntiHoist Entertainment LLC. All rights reserved',
            style: kRobotoText.copyWith(
              fontWeight: kFwMedium,
              color: kColorGrey,
              fontSize: kBodySmall,
            ),
          ),
        ],
      ),
    );
  }
}

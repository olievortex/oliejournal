import 'package:flutter/material.dart';

const helpUrl = "https://kinde.com/docs";
const docsUrl = "https://kinde.com/docs/developer-tools/flutter-sdk/";
const appTitle = "OlieJournal";

/// Text Styles
const TextStyle kRobotoText = TextStyle(fontFamily: 'Roboto');
TextStyle kTitleText = kRobotoText.copyWith(
  fontWeight: kFwMedium,
  color: Colors.black,
  fontSize: kTitle,
);

/// Font Sizes
double kTitle = 24;
double kTitleLarge = 32;
double kHeadingTwo = 20;
double kBodySmall = 12;

/// Font Weights
FontWeight kFwBold = FontWeight.w700;
FontWeight kFwMedium = FontWeight.w500;
FontWeight kFwBlack = FontWeight.w900;

// Horizontal Spacing
Widget horizontalSpaceRegular = SizedBox(width: 16.0);

// Vertical Spacing
Widget verticalSpaceSmall = SizedBox(height: 8.0);
Widget verticalSpaceRegular = SizedBox(height: 16.0);
Widget verticalSpaceMedium = SizedBox(height: 24.0);

// Box Decoration
BoxDecoration roundedBoxRegular = BoxDecoration(
  color: Colors.black,
  borderRadius: BorderRadius.all(Radius.circular(16)),
);

// Color
const kColorGrey = Color(0xFF676767);
const kColorLightGrey = Color(0xFFF7F6F6);

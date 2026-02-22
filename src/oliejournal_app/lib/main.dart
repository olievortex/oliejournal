import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:kinde_flutter_sdk/kinde_flutter_sdk.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/pages/home/home_page.dart';
import 'package:oliejournal_app/pages/tutorial/tutorial_page.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await dotenv.load(fileName: ".env");
  await KindeFlutterSDK.initializeSDK(
    authDomain: dotenv.env['KINDE_AUTH_DOMAIN']!,
    authClientId: dotenv.env['KINDE_AUTH_CLIENT_ID']!,
    loginRedirectUri: dotenv.env['KINDE_LOGIN_REDIRECT_URI']!,
    logoutRedirectUri: dotenv.env['KINDE_LOGOUT_REDIRECT_URI']!,
    audience: dotenv.env['KINDE_AUDIENCE']!,
    scopes: ["email", "profile", "offline", "openid"], // optional
  );

  runApp(
    ChangeNotifierProvider(
      create: (context) => OlieModel(),
      child: const OlieJournalApp(),
    ),
  );
}

class OlieJournalApp extends StatelessWidget {
  const OlieJournalApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: appTitle,
      theme: ThemeData(
        // This is the theme of your application.
        //
        // TRY THIS: Try running your application with "flutter run". You'll see
        // the application has a purple toolbar. Then, without quitting the app,
        // try changing the seedColor in the colorScheme below to Colors.green
        // and then invoke "hot reload" (save your changes or press the "hot
        // reload" button in a Flutter-supported IDE, or press "r" if you used
        // the command line to start the app).
        //
        // Notice that the counter didn't reset back to zero; the application
        // state is not lost during the reload. To reset the state, use hot
        // restart instead.
        //
        // This works for code too, not just values: Most code changes can be
        // tested with just a hot reload.
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
      ),
      home: const _LaunchGate(),
    );
  }
}

class _LaunchGate extends StatefulWidget {
  const _LaunchGate();

  @override
  State<_LaunchGate> createState() => _LaunchGateState();
}

class _LaunchGateState extends State<_LaunchGate> {
  static const String _tutorialCompletedKey = 'tutorial_completed';

  bool? _hasCompletedTutorial;

  @override
  void initState() {
    super.initState();
    _loadTutorialStatus();
  }

  Future<void> _loadTutorialStatus() async {
    final preferences = await SharedPreferences.getInstance();

    if (!mounted) {
      return;
    }

    setState(() {
      _hasCompletedTutorial =
          preferences.getBool(_tutorialCompletedKey) ?? false;
    });
  }

  Future<void> _completeTutorial() async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.setBool(_tutorialCompletedKey, true);

    if (!mounted) {
      return;
    }

    setState(() {
      _hasCompletedTutorial = true;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_hasCompletedTutorial == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_hasCompletedTutorial!) {
      return const HomePage();
    }

    return TutorialPage(onFinished: _completeTutorial);
  }
}

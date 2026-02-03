import 'package:flutter/material.dart';
import 'package:kinde_flutter_sdk/kinde_flutter_sdk.dart';

class OlieModel extends ChangeNotifier {
  bool isLoggedIn = false;
  bool isLoading = false;
  String get fullName {
    return '${_profile?.givenName?[0]}${_profile?.familyName?[0]}';
  }
  UserProfileV2? _profile;

  final KindeFlutterSDK _kindeClient = KindeFlutterSDK.instance;

  OlieModel() {
    _kindeClient.isAuthenticated().then((value) {
      isLoggedIn = value;

      if (value) {
        _getProfile();
      }
    });
  }

  Future<void> onRegister() async {
    try {
      await _kindeClient.register();
    } catch (ex) {
      debugPrint(ex.toString());
    }
  }

  Future<void> onLogin() async {
    String? token;

    isLoading = true;
    notifyListeners();

    try {
      token = await _kindeClient.login(type: AuthFlowType.pkce);
    } catch (ex) {
      debugPrint(ex.toString());
    }

    if (token != null) {
      await _getProfile();
      isLoggedIn = true;
    }

    // fullName = "Dillon McMillon";
    isLoading = false;
    notifyListeners();
  }

  Future<void> onLogout() async {
    isLoading = true;
    notifyListeners();

    await _kindeClient.logout();

    isLoggedIn = false;
    // fullName = "";
    isLoading = false;
    notifyListeners();
  }

  Future<void> _getProfile() async {
    isLoading = true;
    notifyListeners();

    _profile = await _kindeClient.getUserProfileV2();

    isLoading = false;
    notifyListeners();
  }
}

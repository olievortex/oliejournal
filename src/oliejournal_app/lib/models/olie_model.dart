import 'package:flutter/material.dart';
import 'package:kinde_flutter_sdk/kinde_flutter_sdk.dart';
import 'package:oliejournal_app/backend.dart';
import 'package:oliejournal_app/models/forecast_model.dart';

class OlieModel extends ChangeNotifier {
  final KindeFlutterSDK _kindeClient = KindeFlutterSDK.instance;
  UserProfileV2? _profile;

  bool isLoggedIn = false;
  bool isLoading = false;
  String get fullName {
    return '${_profile?.givenName?[0]}${_profile?.familyName?[0]}';
  }

  String? errorMessage;
  ForecastModel? forecast;

  OlieModel() {
    _kindeClient.isAuthenticated().then((value) {
      isLoggedIn = value;

      if (value) {
        _getProfile();
      }
    });
  }

  //region API

  Future<void> fetchForecast() async {
    isLoading = true;
    notifyListeners();

    try {
      forecast = await Backend.fetchForecast();
      errorMessage = null;
    } catch (ex) {
      errorMessage = ex.toString();
      forecast = null;
    }

    isLoading = false;
    notifyListeners();
  }

  //endregion

  //region Authentication

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

    isLoading = false;
    notifyListeners();
  }

  Future<void> onLogout() async {
    isLoading = true;
    notifyListeners();

    await _kindeClient.logout();

    _profile = null;
    isLoggedIn = false;
    isLoading = false;
    notifyListeners();
  }

  Future<void> _getProfile() async {
    _profile = await _kindeClient.getUserProfileV2();
  }

  //endregion
}

import 'package:flutter/material.dart';
import 'package:kinde_flutter_sdk/kinde_flutter_sdk.dart';
import 'package:oliejournal_app/backend.dart';
import 'package:oliejournal_app/models/forecast_model.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';

class OlieModel extends ChangeNotifier {
  final KindeFlutterSDK _kindeClient = KindeFlutterSDK.instance;
  UserProfileV2? _profile;

  bool isLoggedIn = false;
  bool isLoading = false;
  String get nameAbbr {
    return '${_profile?.givenName?[0]}${_profile?.familyName?[0]}';
  }
  String get nameFirst {
    return _profile?.givenName ?? 'anonymous';
  }

  String? token;

  String? errorMessage;
  ForecastModel? forecast;
  List<JournalEntryModel> journalEntries = [];

  OlieModel() {
    _kindeClient.isAuthenticated().then((value) {
      isLoggedIn = value;

      if (value) {
        _getUser();
      }
    });
  }

  //region API

  Future<void> fetchForecast() async {
    isLoading = true;
    notifyListeners();

    try {
      forecast = await Backend.fetchForecast(token);
      errorMessage = null;
    } catch (ex) {
      errorMessage = ex.toString();
      forecast = null;
    }

    isLoading = false;
    notifyListeners();
  }

  Future<void> fetchEntries() async {
    isLoading = true;
    notifyListeners();

    try {
      journalEntries = await Backend.fetchJournalEntries(token);
      errorMessage = null;
    } catch (ex) {
      errorMessage = ex.toString();
      forecast = null;
    }

    isLoading = false;
    notifyListeners();
  }

  Future<void> fetchEntryStatus(int id) async {
    // we intentionally don't toggle isLoading here because it would drive
    // the entire UI busy indicator while just one row is updated.
    try {
      final updated = await Backend.fetchJournalEntry(id, token);
      final idx = journalEntries.indexWhere((e) => e.id == id);
      if (idx != -1) {
        journalEntries[idx] = updated;
        notifyListeners();
      }
    } catch (ex) {
      // no field to surface this at the moment; store for diagnostics
      errorMessage = ex.toString();
    }
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
    isLoading = true;
    notifyListeners();

    if (await _kindeClient.isAuthenticated()) {
      _profile = await _kindeClient.getUserProfileV2();
      token = await _kindeClient.getToken();
      isLoggedIn = true;
    } else {
      try {
        token = await _kindeClient.login(type: AuthFlowType.pkce);

        if (token != null) {
          _profile = await _kindeClient.getUserProfileV2();
          isLoggedIn = true;
        }
      } catch (ex) {
        debugPrint(ex.toString());
      }
    }

    isLoading = false;
    notifyListeners();
  }

  Future<void> onLogout() async {
    isLoading = true;
    notifyListeners();

    await _kindeClient.logout();

    _profile = null;
    token = null;
    isLoggedIn = false;
    isLoading = false;
    notifyListeners();
  }

  Future<void> _getUser() async {
    _profile = await _kindeClient.getUserProfileV2();
    token = await _kindeClient.getToken();

    notifyListeners();
  }

  //endregion
}

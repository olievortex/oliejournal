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
  int journalEntriesCurrentPage = 1;
  int journalEntriesPageSize = 0;
  int journalEntriesTotalItems = 0;
  int journalEntriesTotalPages = 1;
  bool get journalEntriesHasPreviousPage => journalEntriesCurrentPage > 1;
  bool get journalEntriesHasNextPage =>
      journalEntriesCurrentPage < journalEntriesTotalPages;

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

  Future<void> fetchEntries({int? page, int? pageSize}) async {
    isLoading = true;
    notifyListeners();

    try {
      final entriesResult = await Backend.fetchJournalEntries(
        token,
        page: page,
        pageSize: pageSize,
      );
      journalEntries = entriesResult.items;
      journalEntriesCurrentPage = entriesResult.currentPage;
      journalEntriesPageSize = entriesResult.pageSize;
      journalEntriesTotalItems = entriesResult.totalItems;
      journalEntriesTotalPages = entriesResult.totalPages;
      errorMessage = null;
    } catch (ex) {
      errorMessage = ex.toString();
      forecast = null;
    }

    isLoading = false;
    notifyListeners();
  }

  Future<JournalEntryModel?> fetchEntryStatus(int id) async {
    // we intentionally don't toggle isLoading here because it would drive
    // the entire UI busy indicator while just one row is updated.
    try {
      final updated = await Backend.fetchJournalEntry(id, token);
      final idx = journalEntries.indexWhere((e) => e.id == id);
      if (idx != -1) {
        journalEntries[idx] = updated;
        notifyListeners();
      }

      return updated;
    } catch (ex) {
      // no field to surface this at the moment; store for diagnostics
      errorMessage = ex.toString();

      return null;
    }
  }

  //endregion

  //region Authentication

  Future<void> onRegister() async {
    isLoading = true;
    notifyListeners();

    try {
      await _kindeClient.register();

      if (await _kindeClient.isAuthenticated()) {
        _profile = await _kindeClient.getUserProfileV2();
        token = await _kindeClient.getToken();
        isLoggedIn = true;
      }
    } catch (ex) {
      debugPrint(ex.toString());
    } finally {
      isLoading = false;
      notifyListeners();
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
    journalEntries = [];
    journalEntriesCurrentPage = 1;
    journalEntriesPageSize = 0;
    journalEntriesTotalItems = 0;
    journalEntriesTotalPages = 1;
    isLoading = false;
    notifyListeners();
  }

  Future<void> deleteAllUserData() async {
    isLoading = true;
    notifyListeners();

    try {
      await Backend.requestDeleteAllUserData(token);

      journalEntries = [];
      journalEntriesCurrentPage = 1;
      journalEntriesPageSize = 0;
      journalEntriesTotalItems = 0;
      journalEntriesTotalPages = 1;
      forecast = null;
      errorMessage = null;

      await onLogout();
    } catch (ex) {
      errorMessage = ex.toString();
      isLoading = false;
      notifyListeners();
      rethrow;
    }
  }

  Future<void> _getUser() async {
    try {
      _profile = await _kindeClient.getUserProfileV2();
      token = await _kindeClient.getToken();
      notifyListeners();
    } catch (ex) {
      if (_isHttp400(ex)) {
        await onLogout();
        return;
      }

      rethrow;
    }
  }

  bool _isHttp400(Object ex) {
    final message = ex.toString();

    return RegExp(r'\b400\b').hasMatch(message);
  }

  //endregion
}

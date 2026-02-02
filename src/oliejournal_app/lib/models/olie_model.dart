import 'package:flutter/material.dart';

class OlieModel extends ChangeNotifier {
  bool isLoggedIn = false;
  bool isLoading = false;
  String fullName = "";

  Future<void> onRegister() async {
    await onLogin();
  }

  Future<void> onLogin() async {
    isLoading = true;
    notifyListeners();

    await Future.delayed(const Duration(seconds: 2));

    isLoggedIn = true;
    fullName = "Dillon McMillon";
    isLoading = false;
    notifyListeners();
  }

  Future<void> onLogout() async {
    isLoading = true;
    notifyListeners();

    await Future.delayed(const Duration(milliseconds: 500));

    isLoggedIn = false;
    fullName = "";
    isLoading = false;
    notifyListeners();
  }
}
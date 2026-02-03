import 'package:flutter/material.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:provider/provider.dart';

class HomeHeader extends StatelessWidget {
  const HomeHeader({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<OlieModel>(
      builder: (context, olieModel, child) {
        return Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(appTitle, style: kTitleText),
            olieModel.isLoading
                ? const CircularProgressIndicator.adaptive()
                : _trailingWidget(olieModel),
          ],
        );
      },
    );
  }

  Widget _trailingWidget(OlieModel olieModel) {
    return olieModel.isLoggedIn ? _loggedIn(olieModel) : _loggedOut(olieModel);
  }

  Widget _loggedIn(OlieModel olieModel) {
    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          AspectRatio(
            aspectRatio: 1,
            child: ClipOval(
              child: InkWell(
                child: Container(
                  color: kColorLightGrey,
                  alignment: Alignment.center,
                  child: Text(olieModel.fullName, style: kTitleText),
                ),
              ),
            ),
          ),
          horizontalSpaceRegular,
          InkWell(
            onTap: olieModel.onLogout,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Text(
                  olieModel.fullName,
                  style: kRobotoText.copyWith(fontSize: kHeadingTwo),
                ),
                const SizedBox(height: 10),
                const Text('Sign out'),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _loggedOut(OlieModel olieModel) {
    return Row(
      children: [
        MaterialButton(
          padding: EdgeInsets.zero,
          highlightColor: Colors.transparent,
          splashColor: Colors.transparent,
          highlightElevation: 0,
          elevation: 0,
          onPressed: olieModel.onLogin,
          child: Text(
            'Sign in',
            style: kRobotoText.copyWith(fontWeight: kFwBold, color: kColorGrey),
          ),
        ),
        MaterialButton(
          elevation: 0,
          padding: EdgeInsets.zero,
          color: Colors.black,
          onPressed: olieModel.onRegister,
          child: Text(
            'Sign up',
            style: kRobotoText.copyWith(
              fontWeight: kFwBold,
              color: Colors.white,
            ),
          ),
        ),
      ],
    );
  }
}

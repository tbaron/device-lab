package com.infospace.devicelab;

import android.app.Application;
import android.content.Intent;

public class MainApplication
	extends Application {

	@Override
	public void onCreate() {
		super.onCreate();

		startService(new Intent(this, MainService.class));
	}
}

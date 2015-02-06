package com.infospace.devicelab;

import android.content.*;

public class AutoStartReceiver
	extends BroadcastReceiver {

	@Override
	public void onReceive(Context context, Intent intent) {
		context.startService(new Intent(context, MainService.class));
	}

}

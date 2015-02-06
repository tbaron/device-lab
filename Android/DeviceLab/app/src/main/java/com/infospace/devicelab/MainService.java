package com.infospace.devicelab;

import android.app.*;
import android.content.*;
import android.os.*;
import android.os.PowerManager.WakeLock;
import android.support.v4.app.NotificationCompat;

public class MainService
	extends Service {

	public static class ReceiverProxy
		extends BroadcastReceiver {

		@Override
		public void onReceive(Context context, Intent intent) {
			Intent serviceIntent = new Intent(context, MainService.class)
				.setAction(intent.getAction());

			context.startService(serviceIntent);
		}
	}

	public class LocalBinder
		extends Binder {

		MainService getService() {
			return MainService.this;
		}
	}

	private static final float MIN_BATTERY_LEVEL = 10f;
	private static final String WAKE_LOCK_TAG = "DeviceLabWakeLock";

	private static final int NOTIFICATION_ID = R.id.service_main_notification;

	private final IBinder mBinder = new LocalBinder();
	private WakeLock wakeLock;
	private String reason;

	public MainService() {
	}

	@Override
	public void onCreate() {
		super.onCreate();

		if (isConnectedAndCharged()) {
			initWakeLock();
		} else {
			dismissWakeLock();
		}

		update();
	}

	private Notification createNotification() {
		String title = getString(R.string.service_main_title);
		String text = getStatusText();

		PendingIntent intent = PendingIntent.getActivity(
			this, 0, new Intent(this, MainActivity.class), 0);


		return new NotificationCompat.Builder(this)
			.setSmallIcon(R.drawable.ic_service_notification)
			.setTicker(text)
			.setWhen(0)
			.setPriority(NotificationCompat.PRIORITY_MIN)

			.setContentTitle(title)
			.setContentText(text)
			.setContentIntent(intent)

			.build();
	}

	private String getStatusText() {
		if (wakeLock != null) {
			return "Active - " + reason;
		}

		return "Inactive - " + reason;
	}

	private synchronized void initWakeLock() {
		if (wakeLock != null) {
			return;
		}

		PowerManager powerManager = (PowerManager) getSystemService(POWER_SERVICE);

		wakeLock = powerManager.newWakeLock(
			PowerManager.FULL_WAKE_LOCK | PowerManager.ON_AFTER_RELEASE | PowerManager.ACQUIRE_CAUSES_WAKEUP,
			WAKE_LOCK_TAG);
		wakeLock.acquire();
	}

	@Override
	public void onDestroy() {
		dismissWakeLock();
		dismissNotification();

		super.onDestroy();
	}

	private synchronized void dismissWakeLock() {
		if (wakeLock == null) {
			return;
		}

		wakeLock.release();
		wakeLock = null;
	}

	private void dismissNotification() {
		stopForeground(true);
	}

	@Override
	public IBinder onBind(Intent intent) {
		return mBinder;
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		if (intent != null) {
			handleIntent(intent);
		}

		return super.onStartCommand(intent, flags, startId);
	}

	private void handleIntent(Intent intent) {
		String action = intent.getAction();

		if (shouldDeactivate(action)) {
			dismissWakeLock();
		} else if (shouldActivate(action)) {
			initWakeLock();
		}

		update();
	}

	private boolean shouldActivate(String action) {
		if (!isConnectedAndCharged()) {
			return false;
		}

		if (Intent.ACTION_POWER_CONNECTED.equals(action)) {
			reason = "Connected";
			return true;
		}

		if (Intent.ACTION_BATTERY_OKAY.equals(action)) {
			reason = "Battery restored";
			return true;
		}

		return false;
	}

	private boolean shouldDeactivate(String action) {
		if (Intent.ACTION_POWER_DISCONNECTED.equals(action)) {
			reason = "Disconnected";
			return true;
		}

		if (Intent.ACTION_BATTERY_LOW.equals(action)) {
			reason = "Battery low";
			return true;
		}

		return false;
	}

	public boolean isConnectedAndCharged() {
		Intent batteryIntent = registerReceiver(null, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
		boolean connected = batteryIntent.getIntExtra(BatteryManager.EXTRA_PLUGGED, -1) > 0;

		if (!connected) {
			reason = "Disconnected";
			return false;
		}

		if (getBatteryLevel(batteryIntent) < MIN_BATTERY_LEVEL) {
			reason = "Battery low";
			return false;
		}

		reason = "Connected and battery ≥ " + MIN_BATTERY_LEVEL;
		return true;
	}

	private static float getBatteryLevel(Intent batteryIntent) {
		int level = batteryIntent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
		int scale = batteryIntent.getIntExtra(BatteryManager.EXTRA_SCALE, -1);

		return ((float) level / (float) scale) * 100.0f;
	}

	private void update() {
		startForeground(NOTIFICATION_ID, createNotification());
	}
}

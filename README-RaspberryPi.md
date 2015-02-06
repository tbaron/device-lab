# Raspberry Pi

This document discusses the setup of a Raspberry Pi to run the DeviceLab.Service.

## Pre-Setup

0. Install *Arch Linux* on the Pi

0. Update all packages
	```sh
	pacman -Syu
	```

0. Update kernel to pickup wifi bug fixes
	```sh
	pacman -S linux-raspberrypi-latest
	```

0. **(Optional)** Install *rsync* for file uploads
	```sh
	pacman -S rsync
	```

0. Configure and start WiFi

	* Create a *wpa_supplicant* config at `/etc/wpa_supplicant/networks.conf`

	```
	ctrl_interface=/var/run/wpa_supplicant
	eapol_version=1
	fast_reauth=1
	ap_scan=1

	network={
		scan_ssid=1
		ssid="{SSID}"
		psk="{PLAIN TEXT KEY}"
	}
	```

	* Create a profile for *netctl* at: `/etc/netctl/wlan0-{SSID}`

	```
	Description='wlan0 config'
	Interface=wlan0
	Connection=wireless
	Security=wpa-config

	IP=dhcp
	WPAConfigFile=/etc/wpa_supplicant/networks.conf
	```

	* Enable and start the profile

	```sh
	netctl enable wlan0-{SSID}
	netctl start wlan0-{SSID}
	```

0. Add root certificates
	```sh
	mozroots --import --ask-remove
	```

## Setup

0. Install *mono*, *android-tools*, and *android-udev*:
	```sh
	pacman -S mono android-tools
	```

0. **(Not currently needed)** ~~Download the `aapt` ARM binary and push it to the Pi~~
	> ~~https://github.com/skyleecm/android-build-tools-for-arm/tree/master/out/host/linux-arm/bin~~

	```sh
	scp aapt root@{RASPBERRY PI IP ADDRESS}:/usr/local/bin
	```

0. Build *DeviceLab.Service* in *Release* configuration and copy the entire *bin* folder to the Pi
	```sh
	scp -r {PATH TO PROJECT}/bin/Release root@{RASPBERRY PI IP ADDRESS}:/usr/local/lib/device-lab
	```

	**Alternatively** copy only modified files
	```sh
	rsync -ruv --delete {PATH TO PROJECT}/bin/Release root@{RASPBERRY PI IP ADDRESS}:/usr/local/lib/device-lab
	```

0. Register the device lab as a service at `/etc/systemd/system/device-lab.service`
	```
	[Unit]
	Description=InfoSpace Device Lab
	After=network.target

	[Service]
	ExecStart=/usr/bin/mono --runtime=v4.0.30319 /usr/local/lib/device-lab/DeviceLab.Service.exe
	ExecReload=/bin/kill -HUP $MAINPID
	KillMode=process
	Restart=always
	RestartSec=2

	[Install]
	WantedBy=multi-user.target
	```

## Start service

0. Enable the service
	```sh
	systemctl enable device-lab
	```

0. Run the service
	```sh
	systemctl start device-lab
	```

0. Check the service status for success
	```sh
	systemctl status device-lab
	```


## Resources

* Installing Mono on Raspberry Pi
	> http://logicalgenetics.com/raspberry-pi-and-mono-hello-world/

* Building aapt for ARM processors
	> http://www.timelesssky.com/blog/building-android-sdk-build-tools-aapt-for-debian-arm

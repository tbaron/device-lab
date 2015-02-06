Build the `DeviceLab.Service` project using the `Release` configuration.

Copy the binaries to the RaspberryPi
```sh
cd Source/DeviceLab.Service/bin/Release`

rsync -uv * root@RASPBERRYPI_HOSTNAME:/usr/local/lib/device-lab
```

The binaries live in the following directory on the RaspberryPi:
```
/usr/local/lib/device-lab
```

On the RaspberryPi, restart the service and check the log for status:

```sh
systemctl stop device-lab
systemctl start device-lab
```

View the log output, starting with the last 25 lines
```sh
journalctl -u device-lab -n 25 -f
```

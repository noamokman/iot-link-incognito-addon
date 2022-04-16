# iot-link-incognito-addon

iot-link-incognito-addon is an addons for [IoTLink](https://iotlink.gitlab.io/).
It is used to detect open chrome incognito processes and update an MQTT sensor, mainly for home automations

### Configuration

```yaml
# Defualt configuration values
enabled: true # if the addon is enabled
interval: 5000 # interval to check open windows
topic: incognito # the topic name to report to
cacheTTL: 30000 # will update the topic with the same value only once per this TTL
```

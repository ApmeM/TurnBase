# Keystore for android

## Overview

### Configure android deployment keystores

As an example this repository already has 2 prepared keystore files 

- debug_keystore with password debug_password
- release_keystore with password release_password

Debug keystore can be used on local machine to upload apk on android devices.
Release keystore is used to deploy apk to google play.

To make it work add your relese password to `RELEASE_KEYSTORE_PASSWORD` [repository secret](../../settings/secrets/actions)

### Recreate keystores for android deployment

Go to "keystore" folder, remove release.keystore and execute following command:

```
keytool -genkey -v -keystore release.keystore -alias release_user -keyalg RSA -keysize 2048 -validity 10000
```

And then add release keystore password to github secrets with the name RELEASE_KEYSTORE_PASSWORD
You can regenerate debug.keystore as well with the following command (see the password is already set):

```
keytool -genkey -v -keystore debug.keystore -alias debug_user -storepass debug_password -keyalg RSA -keysize 2048 -validity 10000
```

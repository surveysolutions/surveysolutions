# Vagrant

Download here: https://www.vagrantup.com/downloads.html

# Setup

## VAGRANT Home folder

Windows Server require very much space to work. Vagrant by default will use USER home directory for images
Change `VAGRANT_HOME` variable to change default boxes location:

```
Setx VAGRANT_HOME "d:/VM/vagrant"
```
REMEMBER TO RELOAD TERMINAL so that settings are applied

## Vagrant dot file location

Change `VAGRANT_DOTFILE_PATH` environment variable where created disk fill be located.

## HYPER-V

Don't forget to change default `Virtual Hard Disks` location in `Hyper-V Settings...`

# Running

Use terminal with Administrative privilages
`vagrant up`

Enter user/pass to setup

## WAIT... WAIT a LOT

`vagrant rdp`

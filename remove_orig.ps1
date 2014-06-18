get-childitem -exclude .hg -include *.orig -recurse | foreach ($_) {
    "Removing $_.fullname"
    remove-item $_.fullname
} 
get-childitem -exclude .hg -include *.orig -recurse | foreach ($_) {
    "Removing $_"
    remove-item $_.fullname
} 
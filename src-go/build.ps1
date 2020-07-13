if ($args[0] -eq "linux")
{
    $Env:GOOS = "linux";
    $Env:GOARCH = "amd64";

    go build
}

if ($args[0] -eq "win")
{
    $Env:GOOS = "windows";
    $Env:GOARCH = "amd64";

    go build
}
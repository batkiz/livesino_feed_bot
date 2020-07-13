param (
  [String]$Os = $Env:GOOS,
  [String]$Arch = $ENV:GOARCH
)

$DefaultOs = "windows"
$DefaultArch = "amd64"


$Env:GOOS = $Os
$ENV:GOARCH = $Arch

go build

$Env:GOOS = $DefaultOs
$ENV:GOARCH = $DefaultArch

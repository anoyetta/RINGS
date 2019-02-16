$cd = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $cd

# version
$tag = $(Get-Content .\CURRENT_VERSION).Trim("\r").Trim("\n")
Write-Output ("-> " + $tag)

'-> commit'
git commit -a -m $tag

'-> checkout master'
git checkout master

'-> merge develop into master'
git merge develop -m ("Merge branch develop " + $tag) --no-ff

Write-Output ("-> tag " + $tag)
git tag $tag

git checkout develop

'done!'
pause

Write-Output "Runnig AWS Network Tasks";
$gateway = (Get-WMIObject -Class Win32_IP4RouteTable | Where { $_.Destination -eq '0.0.0.0' -and $_.Mask -eq '0.0.0.0' } | Sort-Object Metric1 | Select NextHop).NextHop
$ifIndex = (Get-NetAdapter -InterfaceDescription "*Hyper-V*" | Sort-Object | Select ifIndex).ifIndex
New-NetRoute -DestinationPrefix 169.254.170.2/32 -InterfaceIndex $ifIndex -NextHop $gateway

Write-Output "Starting tcpServer";
./tcpServer.exe;
## Rest Client

[![Actions](https://github.com/wk-j/rest-client/workflows/NuGet/badge.svg)](https://github.com/wk-j/rest-client/actions)
[![NuGet](https://img.shields.io/nuget/v/wk.RestClient.svg)](https://www.nuget.org/packages/wk.RestClient)
[![NuGet Downloads](https://img.shields.io/nuget/dt/wk.RestClient.svg)](https://www.nuget.org/packages/wk.RestClient)

## Installation

```bash
dotnet tool install -g wk.RestClient
```

## Usage

### 1. Create request file `http/regres.in/register.rest`

```
// This is comment
POST https://reqres.in/api/register
Content-Type: application/json
Authorization: Bearer xyz
X-Proxy-Token: e abcd

{
    "email": "eve.holt@reqres.in",
    "password": "pistol"
}
```

### 2. Execute `wk-rest` in terminal

```
> wk-rest --file http/regres.in/register.rest

{
  "id": 4,
  "token": "QpwL5tke4Pnpja7X4"
}
```

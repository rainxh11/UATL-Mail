import { HubConnectionBuilder, LogLevel, IHttpConnectionOptions } from '@microsoft/signalr'
import store from '@/store'
import * as signalR from '@microsoft/signalr'

export const mailHub = (baseDomain) => {

  return new HubConnectionBuilder()
    .withUrl(baseDomain + '/hubs/mail', {
      accessTokenFactory: () => store.getters['auth/getToken']
      ,skipNegotiation: true
      ,transport: signalR.HttpTransportType.WebSockets
    })
    .configureLogging(LogLevel.Information)
    .build()
}

import { HubConnectionBuilder, LogLevel, IHttpConnectionOptions } from '@microsoft/signalr'
import store from '../store'

export const mailHub = (baseDomain) => {
  return new HubConnectionBuilder()
    .withUrl(`${baseDomain}/hubs/mail`, { accessTokenFactory: () => store.getters['auth/getToken']  })
    .configureLogging(LogLevel.Information)
    .build()
}

export const chatHub = (baseDomain) => {
  return new HubConnectionBuilder()
    .withUrl(`${baseDomain}/hubs/chat`, { accessTokenFactory: () => store.getters['auth/getToken'] })
    .configureLogging(LogLevel.Information)
    .build()
}

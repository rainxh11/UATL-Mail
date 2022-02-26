import { HubConnectionBuilder, LogLevel, IHttpConnectionOptions } from '@microsoft/signalr'
import store from '../store'

const mailHub = (baseDomain) => {
  return new HubConnectionBuilder()
    .withUrl(`${baseDomain}/hubs/mail`, { accessTokenFactory: () => store.getters['auth/getToken']  })
    .configureLogging(LogLevel.Information)
    .build()
}

const chatHub = (baseDomain) => {
  return new HubConnectionBuilder()
    .withUrl(`${baseDomain}/hubs/chat`, { accessTokenFactory: () => store.getters['auth/getToken'] })
    .configureLogging(LogLevel.Information)
    .build()
}

export default function createMailHub() {
  return store => {
    //subscribe to events
    mailHub.start()
  }
}

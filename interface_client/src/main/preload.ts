import { contextBridge, ipcRenderer } from 'electron';

const electronHandler = {
  ipcRenderer: {
    sendMessage: (channel: string, msg: string) => ipcRenderer.send(channel, msg),
  },
};

contextBridge.exposeInMainWorld('test_api', electronHandler);

export type ElectronHandler = typeof electronHandler;

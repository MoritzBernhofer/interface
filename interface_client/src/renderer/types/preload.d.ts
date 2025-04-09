export interface ElectronHandler {
  ipcRenderer: {
    sendMessage: (channel: string, msg: string) => void;
  };
}

declare global {
  interface Window {
    test_api: ElectronHandler;
  }
}

export {};

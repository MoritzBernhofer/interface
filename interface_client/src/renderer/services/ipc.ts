export const sendMessage = (msg: string) => {
  window.test_api?.ipcRenderer.sendMessage('sendMessage', msg);
};

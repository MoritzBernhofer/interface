import { ipcMain } from 'electron';

ipcMain.on('sendMessage', (event, msg: string) => {
  console.log(msg);
  console.log('here');
});

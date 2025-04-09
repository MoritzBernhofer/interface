import { app, BrowserWindow } from 'electron';
import { createMainWindow } from './windows/mainWindow';
import './ipc/handlers';

let mainWindow: BrowserWindow;

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

app
  .whenReady()
  .then(async () => {
    mainWindow = await createMainWindow();
    app.on('activate', () => {
      if (mainWindow === null) createMainWindow();
    });
  })
  .catch(console.log);

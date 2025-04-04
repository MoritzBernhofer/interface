import { ElectronHandler } from '../main/preload';

declare global {
  interface Window {
    test_api: ElectronHandler;
  }
}

export {};

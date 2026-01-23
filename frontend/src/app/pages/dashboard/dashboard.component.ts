import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, ClientServerService, IotDeviceService } from '../../services';
import { ClientServerDto, IotDeviceDto } from '../../models';

type SelectedItem = 
  | { type: 'server'; data: ClientServerDto }
  | { type: 'device'; data: IotDeviceDto }
  | null;

interface ServerNode {
  server: ClientServerDto;
  expanded: boolean;
  devices: IotDeviceDto[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private clientServerService = inject(ClientServerService);
  private iotDeviceService = inject(IotDeviceService);

  user = this.authService.user;
  isLoading = signal(true);
  serverNodes = signal<ServerNode[]>([]);
  allDevices = signal<IotDeviceDto[]>([]);
  selectedItem = signal<SelectedItem>(null);

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);

    this.clientServerService.getAll().subscribe({
      next: (servers) => {
        this.iotDeviceService.getAll().subscribe({
          next: (devices) => {
            this.allDevices.set(devices);
            const nodes: ServerNode[] = servers.map(server => ({
              server,
              expanded: false,
              devices: devices.filter(d => d.iotServiceId === server.id)
            }));
            this.serverNodes.set(nodes);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
        });
      },
      error: () => this.isLoading.set(false)
    });
  }

  toggleServer(index: number): void {
    this.serverNodes.update(nodes => {
      const updated = [...nodes];
      updated[index] = { ...updated[index], expanded: !updated[index].expanded };
      return updated;
    });
  }

  selectServer(server: ClientServerDto): void {
    this.selectedItem.set({ type: 'server', data: server });
  }

  selectDevice(device: IotDeviceDto): void {
    this.selectedItem.set({ type: 'device', data: device });
  }

  isServerSelected(serverId: number): boolean {
    const item = this.selectedItem();
    return item?.type === 'server' && item.data.id === serverId;
  }

  isDeviceSelected(deviceId: number): boolean {
    const item = this.selectedItem();
    return item?.type === 'device' && item.data.id === deviceId;
  }

  getDevicesForServer(serverId: number): IotDeviceDto[] {
    return this.allDevices().filter(d => d.iotServiceId === serverId);
  }

  logout(): void {
    this.authService.logout();
  }
}

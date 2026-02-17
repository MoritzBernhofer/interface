export interface IotDeviceDto {
  id: number;
  ipv4: string;
  iotServiceId: number;
  name: string;
}

export interface CreateIotDeviceDto {
  ipv4: string;
  iotServiceId: number;
  name: string;
}

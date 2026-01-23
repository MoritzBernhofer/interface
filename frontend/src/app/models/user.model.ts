export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  token: string;
  userId: number;
  email: string;
  name: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  password: string;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
}

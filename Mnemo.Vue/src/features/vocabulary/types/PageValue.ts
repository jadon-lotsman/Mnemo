export interface PageValue<T = unknown> {
  items: T[]
  page: number
  pageSize: number
  totalPages: number
}

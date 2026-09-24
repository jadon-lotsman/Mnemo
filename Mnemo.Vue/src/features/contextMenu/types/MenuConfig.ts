import type { ContextMenuOption } from './ContextMenuOption'

export interface PagedListModule {
  type: 'search'
  placeholder: string
  options: ContextMenuOption[]
  listSize?: number
}

export interface ListModule {
  type: 'list'
  options: ContextMenuOption[]
}

export interface TextModule {
  type: 'text'
  text: string
}

export type ContextMenuModule = PagedListModule | ListModule | TextModule

export interface MenuConfig {
  modules: ContextMenuModule[]
}

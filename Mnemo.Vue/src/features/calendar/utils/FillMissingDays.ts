import type { RepetitionDay } from '../types/RepetitionDay'

export function fillMissingDays(days: RepetitionDay[]): RepetitionDay[] {
  if (days.length === 0) return []

  const firstDateStr = days[0]?.date
  const lastDateStr = days[days.length - 1]?.date

  const startDate = new Date(firstDateStr ?? '')
  const endDate = new Date(lastDateStr ?? '')

  let diff = Math.round((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))
  if (diff < 5) {
    diff = 5
  }

  const totalDays = diff + 1 + (6 - ((diff + 1) % 6))

  const daysMap = new Map<string, RepetitionDay>()
  for (const day of days) {
    daysMap.set(day.date, day)
  }

  const result: RepetitionDay[] = []
  const current = new Date(startDate)

  for (let i = 0; i < totalDays; i++) {
    const dateStr = formatDate(current)
    const existing = daysMap.get(dateStr)

    if (existing) {
      result.push(existing)
    } else {
      result.push({
        date: dateStr,
        vocabularyForeigns: [],
      })
    }

    current.setDate(current.getDate() + 1)
  }

  return result
}

function formatDate(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

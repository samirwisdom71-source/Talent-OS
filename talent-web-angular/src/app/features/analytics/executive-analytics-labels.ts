/** i18n keys — values align with `TalentSystem.Domain.Enums` numeric ordinals. */

export function performanceBandI18nKey(value: number): string {
  const keys: Record<number, string> = {
    1: 'enums.performanceBand.low',
    2: 'enums.performanceBand.medium',
    3: 'enums.performanceBand.high',
  };
  return keys[value] ?? 'enums.unknown';
}

export function potentialBandI18nKey(value: number): string {
  const keys: Record<number, string> = {
    1: 'enums.potentialBand.low',
    2: 'enums.potentialBand.medium',
    3: 'enums.potentialBand.high',
  };
  return keys[value] ?? 'enums.unknown';
}

export function readinessLevelI18nKey(value: number): string {
  const keys: Record<number, string> = {
    1: 'enums.readinessLevel.readyNow',
    2: 'enums.readinessLevel.readySoon',
    3: 'enums.readinessLevel.readyLater',
  };
  return keys[value] ?? 'enums.unknown';
}

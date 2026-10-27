import { describe, expect, it } from 'vitest';
import { parseRoutineText } from './routine-text-parser';

describe('parseRoutineText', () => {
  it('splits day headings, exercise prescriptions, and notes', () => {
    const result = parseRoutineText(`Día 1 — Pecho, hombros y tríceps
1. Press de banca con barra — 4 x 6-8
2. Press inclinado con mancuernas — 3 x 8-10 — Pausa de 90 segundos

Día 2 — Piernas A
1. Sentadilla con barra o multipower — 4 x 6-8
2. Prensa de piernas — 3x10-12`);

    expect(result).toHaveLength(2);
    expect(result[0]).toMatchObject({
      nombreDia: 'Día 1',
      enfoque: 'Pecho, hombros y tríceps',
      ejercicios: [
        { nombre: 'Press de banca con barra', seriesYRepeticiones: '4 x 6-8', notas: '' },
        { nombre: 'Press inclinado con mancuernas', seriesYRepeticiones: '3 x 8-10', notas: 'Pausa de 90 segundos' },
      ],
    });
    expect(result[1].ejercicios[1]).toMatchObject({
      nombre: 'Prensa de piernas',
      seriesYRepeticiones: '3x10-12',
    });
  });

  it('recognizes weekday headings and keeps unrecognized exercise lines intact', () => {
    const result = parseRoutineText(`**Lunes: Tren superior**
Remo en máquina con control y pausa
Curl de bíceps | 3 × 12 reps | Sin balanceo`);

    expect(result).toHaveLength(1);
    expect(result[0].nombreDia).toBe('Lunes');
    expect(result[0].ejercicios).toEqual([
      { nombre: 'Remo en máquina con control y pausa', seriesYRepeticiones: '', notas: '' },
      { nombre: 'Curl de bíceps', seriesYRepeticiones: '3 × 12 reps', notas: 'Sin balanceo' },
    ]);
  });
});

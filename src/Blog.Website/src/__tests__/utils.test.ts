import { describe, it, expect } from 'vitest';
import { formatDate, truncate } from '@/lib/utils';

describe('utils', () => {
  describe('formatDate', () => {
    it('should return empty string for null', () => {
      expect(formatDate(null)).toBe('');
    });

    it('should format a valid date string', () => {
      const result = formatDate('2024-01-15T00:00:00Z');
      expect(result).toContain('January');
      expect(result).toContain('15');
      expect(result).toContain('2024');
    });
  });

  describe('truncate', () => {
    it('should not truncate short text', () => {
      expect(truncate('hello', 10)).toBe('hello');
    });

    it('should truncate long text and add ellipsis', () => {
      expect(truncate('hello world this is long', 11)).toBe('hello world...');
    });
  });
});

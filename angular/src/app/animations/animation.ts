import { trigger, transition, style, animate } from '@angular/animations';

export const fadeInAnimation = trigger('fadeInAnimation', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('500ms ease-out', style({ opacity: 1 })),
  ]),
]);

export const fadeInFromAboveAnimation = trigger('fadeInFromAboveAnimation', [
  transition(':enter', [
    style({
      opacity: 0,
      transform: 'translateY(-100%)' // Initially positioned above
    }),
    animate('500ms ease-out', style({
      opacity: 1,
      transform: 'translateY(0)' // Moves to its original position
    }))
  ])
]);

export const fadeInCenterAnimation = trigger('fadeInCenterAnimation', [
  transition(':enter', [
    style({
      opacity: 0,
      transform: 'scale(0.9)' // Initial scale down, no translation needed
    }),
    animate('500ms ease-out', style({
      opacity: 1,
      transform: 'scale(1)' // Final scale to original size
    }))
  ])
]);

export const fadeOutAnimation = trigger('fadeOutAnimation', [
  transition(':leave', [
    animate('500ms ease-in', style({ opacity: 0 }))
  ])
]);
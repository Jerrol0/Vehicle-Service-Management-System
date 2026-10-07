import { Directive, ElementRef, OnDestroy, OnInit, output } from '@angular/core';
import { fromEvent, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, takeUntil } from 'rxjs/operators';

@Directive({
  selector: 'input[appDebouncedSearch]',
  standalone: true,
})
export class DebouncedSearchDirective implements OnInit, OnDestroy {
  readonly debouncedSearch = output<string>();

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly elementRef: ElementRef<HTMLInputElement>) {}

  ngOnInit(): void {
    fromEvent<InputEvent>(this.elementRef.nativeElement, 'input')
      .pipe(
        map((event) => (event.target as HTMLInputElement).value),
        debounceTime(400),
        distinctUntilChanged(),
        takeUntil(this.destroy$),
      )
      .subscribe((value) => {
        this.debouncedSearch.emit(value);
      });
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

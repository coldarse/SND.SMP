import { SMPTemplatePage } from './app.po';

describe('SMP App', function() {
  let page: SMPTemplatePage;

  beforeEach(() => {
    page = new SMPTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
